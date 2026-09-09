# Task Contract — NTSD28-B3-C25I-ARMOR-RECOVERY-001

> 状态：`VERIFIED / PROGRAMMATIC_CORE_AND_PLACEMENT / FORMAL_CONTENT_PENDING`
> 依赖：`NTSD28-B3-C25I-ARMOR-RECOVERY-OWNER-AUDIT-001 / VERIFIED`

## 目标

以程序化armor profile fixture实现current-Authority C25i恢复状态机，并在同一live-slot中锁定C25h→C25i→C25j顺序；默认无profile实体保持现有无armor行为。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`：只读profile seam。
- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`：C25i core/placement。
- 新focused test与必要SelfCheck。
- 本Task/Record/Ledger/STATE/handoff/总表/manifests。

## 不变量

- timer/gate/reload严格按Authority；C25i在h后j前。
- 不新增持久carrier；复用已验证两字段。
- profile seam无分配、无singleton、默认false；不猜测或伪造正式armor block。
- 不修改C25g/h/j行为、hit/defense/armor selection、Config/parser/model/Scene/Authority。

## 验收

- test-first覆盖placement、negative/no-profile、timer0、broken countdown、positive-HP hold、relation/hold/type3 gate。
- focused/late/C25/snapshot/checksum/NTSD28 broad/SelfCheck、compile0、Console0、Scene unchanged；不要求Play。

## 回滚

移除profile seam与C25i call/core/test；不得回退armor carriers或C25g/h/j。

## 实施与证据

- `BattleNativeArmorRecoveryKernel`实现timer<0、body gate、timer0 reload、broken countdown、positive armor hold及recover<=0→-1。
- `BattleLateEntityLifecycleModule`在C25h后、C25j前调用C25i；先执行timer/body早退，再读取profile，默认实体的profile seam fail-closed返回false。
- test-first refresh得到8个且仅为kernel缺失的CS0103；首次focused `6899f8637a254c33a134049b268d6470` 4/4，补全relation/hold/type3后`9150aef527e74e8f81c26043489212f0` 5/5。
- 首次联合`a400ce4aa54c444e8d8beffb28906543`暴露补丁误把timer<0 gate放入C25h的14个真实回归；恢复C25h、把gate移入C25i后，原断言不改，`3aac0dfedf62460590fa3efe51a77249` 96/96 PASS。
- NTSD28 broad `37f2585449a449d58af2d1b872baf9f8` 437/437；2026-09-05 11:05:02 SelfCheck PASS；post-clear Console0。
- Scene SHA/length/mtime仍为`0D74E174...D77`/203477/`2026-09-04T13:12:45.1526434Z`；未Play、Authority只读。
- 正式armor block schema/content、spawn初始化与hit消费仍未实现，继续归B5+H/B11。
