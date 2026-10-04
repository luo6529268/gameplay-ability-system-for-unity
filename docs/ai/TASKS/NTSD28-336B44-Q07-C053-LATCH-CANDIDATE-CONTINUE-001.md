# NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001

状态：`RUNTIME_PENDING / SCOPED_GREEN`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C053。仅修共享命中候选消费顺序，不涉及非战斗。原Editor已重新编译并完成受控辅助type3独立默认模式12tick定向Play，正式源码/Unity 261/261 可比数值同，第7tick90伤恢复；前三份RED原件保留。相邻纯角色锁存、自然物理键与其它C053出口仍待。运行内Battle Scene SHA稳定且clean，随后独立查询又dirty而磁盘SHA不变，保留内存内容。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。

2026-10-04 相邻回归只读复核：正式源码 `battle_world_tests.cpp::test_world_special_hit_latch_blocks_queued_standard_character_hit` 仅直接断言一个已排队角色候选被拒且目标HP500；`SimulationTickDriver28` 在此拒绝后没有全攻击者终止标记。Unity现有 `BattleCollisionHitDamagePlayModeProbeEditor` 的 `hitConfirmFirst/Second` 两角色矩阵仍断言两者HP均100，生产循环对每个被锁存的角色候选逐项返回`false`，静态调用链不会因此写伤害。但这不是实际运行该Unity相邻探针；需在原Editor Battle Scene重新clean后单独运行聚焦Play，不能用C053辅助type3 GREEN代替。当前 Scene磁盘包含另一项按钮预览Prefab实例，且Editor内存`isDirty=true`；未保存、切场景或启动Play，保留其内容。

权威：336B44正式根EXE同LFR在tick7记录slot50候选0→角色slot0 `rejected`、候选1→type3 slot2 `applied/90伤`；正式playable `BattleWorld28::classify_ordinary_hit_eligibility` 在`special_hit_latch_0eb && target->object_type==0`只返回拒绝，`SimulationTickDriver28`只在`terminates_attacker_invocation`等明确条件下跳出候选循环。本拒绝分支没有设置终止标记。Unity `BattleHitCandidateSequenceRunner.TryConsumeCandidate`相同锁存/角色条件却返回`true`，由`TryConsumeCaptured`解释为`break`。

RED：原Battle Scene受控OID65/OID702/OID251完整Driver，前6tick所选23字段同；第7tick正式slot2 OID251动作20/HP368，Unity动作10/HP458；默认模式与ShadowCompare均复现。第三轮计划已捕获Unity slot50候选0→slot0、候选1→slot2，二者未进入预处理。原件见[场景报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。此证据支持候选级终止差异；具体修复须GREEN确认。

仅改 `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs` 锁存角色分支使其跳过当前候选并继续后继；同步更正 `Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs` 中旧“终止整次攻击者调用”断言文字，保持现有两角色目标实际断言。无OID、技能、角色专用分支；不改DAT、角色图、Scene、Prefab、GameConfig、相机、非战斗、命中其他阶段或Unity框架。

风险：后继候选原来被Unity提前跳过，修正后可能多命中；需核对既有纯角色双候选仍均被拒，以及正式可达“角色候选后接非角色候选”恢复一次90伤且RNG与源码同tick。验证顺序：生成Editor工程编译0错、现有聚焦锁存用例/自检、原Editor已保存且clean后仅C053定向原Scene Play，12tick×所选字段对照正式源码/根、退出clean与Scene SHA、Change Ledger。当前原Editor Battle Scene`isDirty=true`且磁盘SHA在第三次Play退出后外部变化，未取得clean前置前不请求Play。回滚只审本ID两处脚本和文档的局部diff，既有脏Scene及用户内容不回退、不覆盖。
