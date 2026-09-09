# Task Contract — NTSD28-B3-C25F-J-STATE-CARRIERS-001

> 状态：`VERIFIED / STATE_CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHMS-EXCLUDED`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25f-j`
> 依赖：`NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001 / VERIFIED`

## 目标

新增C25f/h/i缺失的12个实体状态载体，闭合default/reset/canonical copy/entity snapshot/full snapshot/checksum/parity，并把raw armor两字段从null提升为真实绑定；不执行computer/timer/poison/join/armor算法。

## 字段

- bank/status：`HitResourceInjuryDouble1A0`、`DelayTimer134`、`JoinTimer148`、`PoisonTimer120`、`PoisonType124`、`PoisonStrength128`、`JoinOverrideActive170`、`JoinOriginalBattleGroup174`、`NativeComputerState1B8`、`NativeTimer1BC`。
- armor：`RuntimeArmorHp118=0`、`ArmorRecoveryTimer11C=-1`。
- `render_phase_008`复用已验证`HitStop`，不得新增重复carrier。

## 允许路径

- `NTSDEntityRuntime`、entity/full snapshot schema、lockstep checksum与parity。
- Unity raw entity projector与`EntityFieldContract`中armor两绑定。
- 新focused test及受schema/count变化影响的既有精确断言。
- 本Task/Record、Ledger、STATE、handoff、总表与C25f-j manifest。

## 不做

- 不连接C25f refresh、C25h timers/status/poison、C25i armor recovery或C25j rest writer。
- 不修改AI consumer、hit producer、DAT parser/model/converter、Config/Scene/Prefab/Authority。
- 不因Authority armor/poison/delay存在而绕过H/B11内容策略gate。

## 验收

- missing-field/schema test-first红灯。
- 12字段默认、Reset、copy、snapshot及checksum全覆盖；parity独立`nativeReactionStatus`组。
- entity/full/checksum schema 6/10/13；raw armor真实输出0/-1，48-field maturity为41 verified/7 missing。
- compile0、focused、snapshot/checksum、raw、NTSD28 broad、SelfCheck通过；无Play需求，Scene unchanged、Console0。

## 回滚

删除12字段和测试，恢复schema5/9/12、raw armor null及39/9 maturity；不得回退render-phase或C25c-e carriers。

## 实施与证据

- test-first取得60个预期missing-field编译错误，均来自新载体测试。
- 12字段进入initializer/default、`ResetNativeReactionStatusCarriers`、full Reset和canonical deep copy。
- entity/full/checksum schema升级6/10/13；checksum逐字段包含，parity新增独立`nativeReactionStatus`组。
- raw armor两字段由null提升为`RuntimeArmorHp118/ArmorRecoveryTimer11C`，48-field maturity变为41 verified/7 missing。
- focused/snapshot/checksum/raw联合job `1e376912240c4467affabdd880a031c9` 66/66 PASS；NTSD28 broad `27ba64974f82438287ae8feed93f6155` 410/410 PASS。
- tool最终顺序执行build0/0、trace self-test21/21、raw self-test5/5；一次并行调用因共享obj文件锁失败，已如实排除并用顺序运行复核。
- 双端raw仍为3tick/6pair/288 occurrences；armor两字段进入33个equal fields，unique differences 17→15，report SHA`125A0697...9C4D`。
- final SelfCheck 2026-09-05 09:11:30 PASS；Scene SHA`0D74E174...D77`不变、未Play、post-clear Console0。
- computer refresh、timer/status/poison/join cleanup、armor recovery与attacker rest算法均未启用。
