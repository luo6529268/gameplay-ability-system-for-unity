# Task Contract — NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B5-EXIT-GATE-AUDIT-001 / VERIFIED`；当前 B6 runtime-pending 栈保持不变

## 目标

穷尽 Unity 生产脚本对旧 `LF2_19 properties.js` 派生 `NTSDSpec` 的直接引用，将每个调用映射到
NTSD 2.8-Logan playable live path、当前/发行语料可达性和正确生产 owner，并冻结后续最小实施包。
本审计不把旧表值改写成新的硬编码表，也不修改脚本、DAT、Scene、Prefab、资源或 Authority。

## Authority 边界

- 正式 EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`；
  `build.ps1 -Target playable` 明确编入 `input_routing.cpp`、`physics_integrator.cpp`、
  `battle_world.cpp` 和 `render_snapshot.cpp`。
- 全 closure 的 `*.cpp/*.h` 对独立单词 `mass` 和 `oscillat*` 均为零命中。这是“当前重建 live path
  没有这两个旧 properties 字段”的已观察事实，不扩大为对所有可能外部二进制符号的证明。
- 角色/持有对象输入动作由 `input_routing.cpp` 的 `interaction_state` 与 linked definition `<stats>`
  字段选择；字段为零或缺失时使用各 call-site fallback。它不是 object-ID 布尔属性表。
- kind 10/11/17/18 冲击由 `battle_world.cpp::resolve_special_relation_hit()` 的 target type、
  `environment_state_320`、owner chain、system `weapon_flute_sky` 表和固定冲击常量裁决；它不读取
  `mass` 或 `oscillate`。Unity 当前相应 hit resolver 的旧 flute 行为是另一项真实差异，不能用删除
  `NTSDSpec` 引用掩盖，须独立审计和实现。

## Unity 直接引用穷尽

`rg -n 'NTSDSpec\.' Assets/NTSD/Scripts --glob '*.cs'` 得到 6 行、7 个调用表达式，分布于 4 个生产文件：

1. `LF2Character.Initialize(...)`：一次 `GetMassOrDefault(characterId)`；
2. `LF2Entity.FluteForce()`：一次 `GetMassOrDefault(ObjectId)`；
3. `LF2CharacterWeaponLinkResolver`：`IsWeaponAttackable`、`CanJustThrowWeapon`、
   `CanStandThrowWeapon`、`CanRunThrowWeapon` 四个表达式；
4. `LF2LivingObject.EffectCreate()`：一次 `GetOscillateOrDefault(effectId)`。

其余 `CanHeavyWeaponDash`、`CanHeavyWeaponJump`、`GetItrZWidthOrDefault` 与表访问 API 没有外部生产
调用者。全仓搜索还确认 `FluteForce()` 与 `EffectCreate()` 只有声明/override，没有脚本、Scene、Prefab
或序列化方法名调用；当前只能标为 repo-closure 内 `NO_PRODUCTION_PRODUCER_OBSERVED`，不能声称第三方
程序集永远不会反射调用公共 API。

## 逐组映射与可达性

### 1. Character mass

- `LF2Character._mass` 仅传入 `CharacterMechanicsContext`，唯一分支是 grounded friction 的
  `mass > 0` gate；ECS character pass同样读取该值。
- Authority `PhysicsIntegrator28::step()` 在 `position.y >= collision_y_reference` 时无条件对 X/Z
  各向零移动一单位，不存在 mass gate。
- `NTSDSpec` 只有旧 ID 1/30 是 character entry，二者 `Mass=null`；所有当前 Direction-B type-0
  character 初始化都得到默认 `1f`。因此正式当前语料下该 gate 恒真，旧表没有改变当前结果；
  但 private/snapshot synthetic mass=0/negative 可制造 Authority 不存在的行为。
- 正确 owner 是 character mechanics + character shell snapshot，不是 object-ID 属性表。

### 2. Held weapon attack/throw booleans

- 合法关系生产值为 `1/2/4/6/101`；heavy `2` 有独立路线，其余路线由 link state选择 linked DAT
  `normal_attack1/2`、`light_throw`、`weapon_drink`、`heavy_throw`、`run_heavy_throw`、`run_attack`、
  `jump_attack`、`sky_light_throw`，再使用 native fallback actions。
- canonical `BattleCharacterActionWriter` 已实现该 selector；旧布尔查询只位于
  `LF2CharacterActionResolver` compatibility fallback及异常/不完整关系兜底，不裁决 canonical profile。
- 对当前138-DAT与release catalog逐definition扫描，上述9个 `<stats>` action字段均为0个实际entry；
  两端正式内容都走 native fallback。现有 synthetic parser/selector tests仍证明非零字段的通用规则。
- compatibility path仍须迁移或退休，不能因为当前/发行语料字段为零而保留旧 table。

### 3. Flute mass

- `LF2Entity.FluteForce()` 在repo closure没有 caller；`LF2WeaponBase`还override为空。
- 真正 hit route直接在 character/weapon resolver、`BattleDamageWriter` 与 HitPlan处理kind10/11，且当前
  实现仍带NTSD 2.4式 `WeaponCount`/combo/stat/flute语义，与2.8冲击事务不同。
- 当前 Direction-B ITR中kind10/11/17/18为0；release有kind10=121、kind11=61、kind17/18=0。
  因而旧 `FluteForce()` 本身是 dormant API，但2.8 kind10/11规则具有release-corpus witness，必须独立
  production package，不能将“dead method”误写为“impact family aligned”。

### 4. Effect oscillate

- `LF2LivingObject.EffectCreate()` 是唯一 `GetOscillateOrDefault` caller，但repo closure没有生产 caller。
- Unity effect snapshot/update仍能由test或restore人工携带 `Oscillate`，不等于存在正式 producer。
- Authority body shake读取 `render_phase_008` 与逻辑tick奇偶；shadow/presentation另读其阈值，并非
  object ID 300/302 的 oscillate table。正确 owner属于B9/B10 render-phase/presentation审计。

### 5. ID alias风险

旧表名称与当前catalog已失去身份同一性：例如旧ID100“棒球棒”在Unity为type4 heal scroll，旧ID101
“镐头”在Unity/release为type4 tag，旧ID201在Unity为death object，旧ID213在Unity/release为puppet，
旧ID300/302也已分别映射criminal/special objects。即使某个旧值偶然与fallback相同，也禁止继续按ID读取。

## 后续五包

1. `NTSD28-B6-NTSDSPEC-MASS-CARRIER-RETIREMENT-PRODUCTION-001`
   - 移除 character初始化的旧表读取；把ground friction固定为Authority无条件规则；
   - 审计并最小迁移 `_mass`、`CharacterMechanicsContext.mass`、character shell snapshot及其tests；
   - 如保留reserved snapshot字段，必须明确只为schema兼容且不得进入规则。
2. `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`
   - compatibility resolver改为与canonical共用link-state + linked-DAT action selector；
   - 删除四个旧布尔调用及中间wrapper；覆盖合法关系与malformed relation no-op，不复活object-ID flags。
3. `NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001`
   - 独立冻结character/object、kind11 environment gate、owner-chain attribution、system immunity、
     fixed motion/action/pending impulse及HitPlan；不能与dead `FluteForce()`删除混为一包。
4. `NTSD28-B9-NTSDSPEC-OSCILLATE-PRODUCER-RETIREMENT-001`
   - 在render-phase/presentation owner审计后移除 `EffectCreate()` 的旧ID lookup；
   - 保留/删除 dormant effect carrier须以实际producer、snapshot兼容和表现验收决定。
5. `NTSD28-B6-NTSDSPEC-EMPTY-SHELL-DISPOSITION-001`
   - 前四项令生产引用归零后，再复扫全仓、asmdef/serialization与public API；
   - 依据用户既定“先迁调用方、再决定空壳”合同决定删除 `NTSDSpec.cs` 或留下无规则兼容壳。

## 验收合同

- 每包独立Task/Change Record，先RED或静态负证据，再最小实现。
- mass：grounded positive/zero/negative synthetic不再改变Authority friction；current formal character结果不变；
  snapshot capture/restore/schema与checksum明确。
- weapon：link1/2/4/6/101在standing/running/state4/state5、direction组合、zero/nonzero linked stats、
  missing/stale target、real/generic shell和legacy/canonical profile对齐；RNG call-site及次数不漂移。
- impact：release kind10/11 witness与synthetic17/18、character/type1/2/4/6/unsupported type、kind11 gate、
  immune/unlisted/empty-table、owner chain、action respond、motion/Y clamp、pending impulse、actual/HitPlan一致。
- oscillate：证明没有旧ID producer残留；body shake/phase以B9/B10新权威表现合同验收。
- 最终 `rg 'NTSDSpec\.'` 外部调用为零，isolated compile、focused、B6/NTSD28、SelfCheck、必要Play及
  first-difference证据通过后，才可处置空壳。

## 不变量 / 阻塞

- 当前 `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001` 与
  `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001` 都是代码已写但Unity runtime未启动；不叠加上述
  production修改。
- 不重试已穷尽的Unity许可路径；只有外部license/Editor状态改变才恢复runtime验收。
- 不修改Direction-B Config、release DAT、Scene/Prefab、资源、Authority目录或正式EXE。
- 不把zero-corpus、dead caller、isolated compile或synthetic test扩大成正式玩法已对齐。

## 回滚

本审计仅新增/更新治理文档；回滚只移除本记录及对应Ledger/STATE/handoff/总表增量。

## Current corpus correction（2026-09-08）

current kind10/11/17/18由`0/0/0/0`纠正为`15/5/0/0`，均为OID36 Tayuya243..247。
NTSDSpec owner inventory与impact三包拆分不变；impact 10/11从release-only改为current reachable。
