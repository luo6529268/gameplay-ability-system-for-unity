# Handoff — R5-CPT-02 CPoint global kill/damage statistics

> 日期：2026-08-22  
> Change ID：R5-CPT-002  
> 当前状态：RUNTIME_PENDING — 最小 production writer、focused self-check、Unity compile与full self-check已通过；C++ trace / Play Mode待验。  
> Authority：J:/QQFile/NTSD2.4/ntsd_release/src/entity/weapon.cpp:42-75、
> src/entity/entity_collision.cpp:57-61，release participation见 Makefile:20-21。

## 已确认

- C++ current held CPoint injury 的 global kill/damage stats 是 weapon.cpp 的 side effect；
- index 仅 1/2 有效；
- holder inactive/missing 只跳过 holder-local score，不跳过有效 index 的 global stats；
- R5-CPT-004 已将 Unity held injury owner收敛到 current SyncHeldCpoint，
  故本包不再有 phase double-injury 前置阻塞。

## 已写内容

1. lethal branch的 holder score 后写 valid index world.KillStats；
2. existing holder combo 后写 valid index world.DamageStats 加 actualInjury；
3. 无 holder 时仍允许 valid global write；
4. shared-DAT lethal assertion与六类 stat matrix已更新；
5. 未改任何 other CPoint path。

## 实际验证

- UnityMCP scripts refresh后 filtered C# compiler error 为 0；
- full BattleRuntimeSelfCheck 的结果文件于 2026-08-22 09:44:35 为 PASS；
- shared lethal和六类 stat matrix均在同次 self-check通过；
- 最终 ledger / scoped diff 已在本次文档收口后重跑并通过：35 条 Record、26 个 governed code file均被覆盖。

## 下一动作

按连续队列预检 D-CPT-003 reciprocal mismatch control flow。它必须保持独立，不得把本包的
global stat结果扩大为完整 CPoint、R5或C++ runtime对齐。

## 未关闭项 / 停止条件

- C++ runtime trace仍由 R1-WP02 阻塞；
- real Unity Play Mode待用户后续认证；
- 若需改 array capacity、pass order、other hit writer、D-CPT-003或任一 scope 外模块，停止并新建合同；
- 当前禁止修改或运行 C++ authority。
