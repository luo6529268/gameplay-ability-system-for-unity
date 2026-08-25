# Handoff — R5-CPT-04 CPoint injury phase ownership

> 日期：2026-08-22  
> Change ID：`R5-CPT-004`  
> 当前状态：`RUNTIME_PENDING` — source、minimal owner transfer、joint fixture、Unity compile和full self-check均已完成；C++ trace / Play Mode待验。  
> Authority：`J:/QQFile/NTSD2.4/ntsd_release/src/entity/game_tick.cpp:659-664`、
> `src/entity/cpoint.cpp:23-190`、`src/entity/weapon.cpp:13-107`。

## Source contract

- C++ cpoint pass没有injury/vaction/position writer；
- current-frame weapon-sync是唯一held CPoint injury/vaction/position writer；
- Unity原先在RunKind1提前调用SyncCaughtByCpoint，later SyncHeldCpoint又可调用一次；
- action可清attacking，故early + later不是可忽略的冗余。

## 已写最小改动

- 从 `BattleCpointWriter.RunKind1` 移除early `SyncCaughtByCpoint`；
- current `SyncHeldCpoint` 未改，继续承担唯一owner；
- existing shared-DAT fixture现在要求RunCpoint后不伤害；
- 新 joint fixture直接走 `PreInteractionTickAll`，覆盖no-action state9一次、action→state9一次、
  action→non-state9零次，并锁定frame、HP/HPBound、combo、attacking、frame-delay、link、position和global stat
  未写边界。

## 首次 Unity 验证发现

首次 full self-check于09:25:20在decrease escape的横向击飞断言失败。旧fixture依赖已删除的
early held-position sync而期望 `+4`；C++ `cpoint.cpp:73` 使用初始X=30/10应得 `-4`。
已在本Record范围内更新三个相关velocity assertion，重新编译后filtered `error CS`=0，
`Temp/NTSD_BattleRuntimeSelfCheck.result` 于09:27:38为`PASS`。

## 明确未改

不改 global stats（D-CPT-002）、reciprocal mismatch（D-CPT-003）、PreInteraction pass order、kind2
validation、throw、held/link、opoint、input、collision、render、DAT/scene或C++ authority。

## 下一步

保持本包为等待 C++ trace / Play Mode 的证据包。按D-009，下一项可建立`D-CPT-002` global stats
独立合同；不得把stats或reciprocal mismatch合并回本包。
