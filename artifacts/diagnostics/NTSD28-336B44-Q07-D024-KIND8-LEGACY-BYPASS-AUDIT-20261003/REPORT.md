# Q07/D-024 kind8 旧直调分支的生产路径核查

日期：2026-10-03。状态：`STATIC_CANDIDATE_ROUTE_AUDIT / SCENE_PENDING`。本项只读当前 Unity 源码与既有 336B44 正式根证据；未改战斗脚本、DAT、图片、Scene、Editor 状态或用户文件。

## 发现与调用链

`LF2CharacterDatHitResolver.ResolveHit` 和 `LF2CharacterHitResolver.ResolveHit` 仍各有 `itr.kind == 8` 后将攻击者物理 Z 写成受害者物理 Z+1 的旧直调代码，分别在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs:447-460` 和 `LF2CharacterHitResolver.cs:203-217`。这两段代码的存在不能直接证明当前生产完整 tick 仍使用它们处理 kind8。

当前候选链的静态路由如下：

1. `LF2CharacterDatHitResolver.cs:60-99` 的 `ResolveCandidateDisposition` 把 kind8 映射为 `Kind8`；同文件 `IsAttackDisposition` 接受它。
2. `BattleHitCandidateSequenceRunner.cs:203-237,350-366` 在通用候选循环内，`Kind8` 先调用 `BattleKind8ControlRelationWriter.TryApply`，不会走该分支的 `consumer.Dispatch`。角色、DAT 对象、武器和特殊攻击的现有候选消费入口均使用这个 runner；数据导向 `BattleInteractionPipeline.cs:430,502` 也调用其 captured 路径。
3. `BattleKind8ControlRelationWriter.cs:59-60` 的当前实际写者已通过 `world.SpatialProjection.SourceDeltaToViewZ(1.0)` 换算物理纵深；`BattleEcsHitExecutionPlan.cs` 中独立的计划投影出口也在同一 Q07/D-024 包修正。原 Editor 具名新例和相邻例合计 8/8 PASS，详[已有包报告](../NTSD28-336B44-Q07-D024-KIND8-VIEW-DELTA-001/REPORT.md)。
4. `LF2Character.cs:57,115` 只构造 `_hitResolver`，当前项目内没有其它该字段读取；`LF2Weapon.ProcessAttack` 的直调角色 `Hit` 路径在 `LF2Weapon.cs:422-438` 构造的是 kind0 持有武器攻击。`BattleDamageWriter.TryApplyCurrentDatTargetHit` 仍是可直接调用的 API，因此本次不宣称所有外部/测试直调永远不可能到达旧分支。

结论仅限于**已检查的生产候选路线**：旧 `+1` 不构成对这条完整 tick kind8 路线的新首差证据，不能因为文本搜索命中就扩成第三处生产修复。若后续原 Battle Scene 近 X480／远 X1200 完整 Driver 或其它已证直调入口实际出现差异，再依据运行时调用者建立独立 Task/Change。当前正式近远根入口已证，原 Battle Scene 的源/物理 Z、碰撞、Game View 和退出保护仍待；Q07/Q09/Q12及总目标不关闭。
