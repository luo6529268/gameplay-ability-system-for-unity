# Task Contract — NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001

> 状态：`VERIFIED / SYSTEM_TABLE_ATTACKER_TERMINAL_ALIGNED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-009 / VERIFIED`

## 目标

把正式system-DAT `john_biscuit={214}`与`henry_arrow={201}`的攻击者终结规则迁入
`BattleDamageWriter`已确定的unarmored character-hit分支，删除`LF2SpecialAttack`无route信息的旧尾部，
使selected-armor与guarded reduced hit不会误触发，并保持OID214/201各自的权威顺序。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5SystemTableAttackerTerminalProductionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task、Change Record、Ledger、STATE、handoff与NTSD 2.8总表

## 不变量

- 仅kind0、type0 character target、unarmored continuation可消费201/214表。
- OID214 HP清零发生在type3 attacker post-hit action之后、后续target/effect tail之前。
- OID201只在unarmored writer尾部释放slot；不得早于该writer剩余可观察尾部。
- broken type1 armor fallback属于unarmored continuation，仍允许两张表；selected active armor和普通防御
  reduced path必须排除。
- 非character、OID300 redirect、非kind0、未应用命中以及非201/214攻击者保持现状。
- DataOriented/ShadowCompare final state与lifecycle observation保持一致；不得改Scene、Config、资源、
  Slot容量、33/3ms或Authority目录。

## TEST-FIRST与验收

- 先新增focused RED：分别证明OID201与214会错误地在type1 armor reduced hit后终结自己。
- GREEN覆盖201/214 unarmored、selected armor reduced、ordinary-defense reduced、broken-armor fallback，
  并至少覆盖ShadowCompare/DataOriented现有投影边界。
- 编译0 error；focused、HitPlan、B5、NTSD28、SelfCheck通过；按风险执行真实Play或明确记录无需新增
  独立视觉见证的理由；Console0、Scene未保存且哈希/dirty/root保持。
- 运行`Tools/Validate-ChangeLedger.ps1`。

## 回滚

恢复`LF2SpecialAttack`旧post-hit helper与调用，撤销`BattleDamageWriter`两处system-table消费、本focused
fixture与SelfCheck定向迁移；不回退其他B5实现。

## 最终结果

OID214已在unarmored writer中按Authority早期位置清零；OID201由预解析unarmored route的special shell
保留至Unity `RecordKind0Hit`尾结束后释放。selected armor与ordinary defense reduced path均排除，broken
armor fallback仍消费表。focused11、HitPlan185、B5 149、NTSD28 330、SelfCheck PASS；Console0，Scene
dirty=false/root13/SHA不变。
