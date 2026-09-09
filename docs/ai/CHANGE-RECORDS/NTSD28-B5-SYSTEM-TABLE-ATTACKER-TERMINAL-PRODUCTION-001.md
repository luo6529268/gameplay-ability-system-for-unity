# NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001
status: VERIFIED
change-kind: BATTLE_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SystemTableAttackerTerminalProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_confirmed_unarmored_hit system-DAT john_biscuit/henry_arrow consumers; LockedSystemBattleTables2833 214/201; EXE B1E13AE1, closure 39DDDA15.
evidence: two leakage REDs plus retained-slot tail RED; final focused11, HitPlan185, B5-149, NTSD28-330, SelfCheck PASS at 18:45:27Z, Console0, Scene unchanged.
-->

> 状态：`VERIFIED / SYSTEM_TABLE_ATTACKER_TERMINAL_ALIGNED`

## Authority与Unity原状

- Authority：unarmored/type0 continuation在attacker post-hit action后读取`john_biscuit={214}`并立即清零
  attacker HP；`henry_arrow={201}`仅记录延迟释放，在audio/spark tail后despawn。reduced hit不消费两表。
- Unity：`LF2SpecialAttack.TryApplyHit`在`DamageWriter`返回true后统一调用`ApplyPostHitSelfDestruct`，无法
  区分unarmored/reduced route；因此201/214泄漏到护甲/防御减伤分支，214时点也晚于权威。
- 既有HitPlan只投影unarmored route的201 lifecycle/214 HP最终值；缺reduced exclusion测试。

## 实际修改

- `BattleDamageWriter.ApplyStandardCharacterDamage`在unarmored branch的type3 attacker post-hit action之后
  清零OID214 HP，早于后续fall/rest/effect和外层hit-record尾；reduced early return不会消费。
- `LF2SpecialAttack.TryApplyHit`在写入前用同一route resolver冻结OID201是否属于unarmored continuation；
  writer与外层`RecordKind0Hit`全部完成后才释放。旧只看`applied && kind0`的helper已删除。
- 顺序RED证明不能在writer内过早释放201；最终保留Unity hit-record owner/tie-break所需slot身份。
- focused fixture覆盖201/214 selected armor、ordinary defense、broken armor fallback、unarmored、slot-retained
  hit-record tail以及DataOriented/ShadowCompare。
- SelfCheck从旧私有helper反射迁到正式`TryApplyHit`入口，继续验证方向不反转。

## 验证

- TEST-FIRST RED：job `8b5f9944f6c043ff961138481c8b0d53`，2/2按预期失败；OID201 selected-armor
  reduced hit后slot为`-1`（预期保持`0`），OID214 HP为`0`（预期保持`100`）。目标命中本包确认的
  route leakage，允许开始最小production迁移。
- 顺序精化RED：初版把OID201释放直接放在`ApplyStandardCharacterDamage`尾部后，job
  `5c9c183b0e90452d84ee70b3d2b45b4d`准确发现Unity外层`RecordKind0Hit`尚未执行；attacker slot较大时
  hit-record owner错误落到target（attacker count预期1、实际0）。因此201必须保留到整个Unity hit-record
  tail结束，再由具备预解析unarmored route的special shell释放；214仍留在writer内的权威早期位置。
- 初版green（2项）：job `6d1f4886abfd4382b125fc41baf15244`，2/2。
- 最终focused：job `66526afdd66b4f67a6e29d88c88435c9`，11/11。
- HitPlan：job `aa1844ad71fd4c7dbd3bc1273583a3fb`，185/185。
- B5：job `29d1f1697c0544ab9d5821ce18fc5101`，149/149。
- NTSD28：job `ddc66385c4c9482db31f4c949e76822a`，330/330。
- 编译：最终domain reload成功，上述测试均被发现执行，Unity脚本0 compile error。
- SelfCheck：请求文件机制于`2026-09-06T18:45:27Z`写出`PASS`；菜单桥接曾超时重试并产生工具层
  disposed-object日志，最终清空Console后error=0，未作为产品失败传播。
- 本逻辑无Mono/Scene/渲染边界；focused直接走正式`LF2SpecialAttack.TryApplyHit`、SimulationWorld、
  DamageWriter与HitPlan两模式，故无需为同一纯战斗写面新增独立视觉Play fixture。
- Scene：`NTSD_Battle` dirty=false、rootCount=13，SHA-256仍为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`；未进入Play、未保存Scene。
- scoped `git diff --check`通过；Change Ledger validator PASS，343 Records / 298 governed code files。

## 回滚

按Task Contract仅撤销本包三个脚本路径增量。
