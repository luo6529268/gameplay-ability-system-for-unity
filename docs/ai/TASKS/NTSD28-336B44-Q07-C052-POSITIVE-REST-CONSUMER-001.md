# Q07/C052 effect21 正命中间隔继续后继候选

状态：`RUNTIME_PENDING / SCOPED_SCENE_PASS`。当前正式 336B44 playable 的 `battle_world.cpp::classify_ordinary_hit_eligibility` 只在 effect21、目标当前 state18/19 且该攻击者对目标的 relation victim rest **等于零**时终止整次攻击。正式 OID211/action161 三实体完整 `GameSession28::step` 的第1 tick 候选为首目标 applied、同目标正rest rejected/terminates0、第二目标 applied；两名 OID2 目标均 HP500→420。原 Unity Battle Scene 同条件使用生产 Driver，第一名 HP420/rest44，第二名 HP500/rest0；远距 X650 第二名 HP500，均完成3tick/退出clean/四保护SHA稳定。[源报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/REPORT.md)、[原Scene证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-UNITY-SCENE-001/REPORT.md)。

仅修改 `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs` 的共享 effect21 提前终止条件，补上 `world.GetRawRestVrest(targetSlot, attackerSlot) == 0`，保持先于普通 prelude 的正式顺序。**不**对 OID211、角色、动作、特定效果组合写特殊分支，不改 DAT/图片/场景/非战斗。测试脚本 `NTSD28Q07C052PositiveRestBattlePlayProbeEditor.cs` 仅在既有独立诊断 Record 下允许 v2 唯一结果 ID；新旧结果不可覆盖。

验收：原 Editor 编译0错；原 Battle Scene 近 X530 与远 X650 各3完整 Driver tick，近第1 tick 双HP420/双rest44，远第1 tick 仅首目标HP420；两次退出Play/Scene clean/四保护SHA不变；原 zero-rest/state18 终止反例和邻近效应聚焦测试通过；`Tools/Validate-ChangeLedger.ps1` 通过。根正式EXE三实体近远已验证选定30/30字段；自然OPoint/物理键仍另列，不因本包自动关闭 C052/Q07。若生产修复引出后继碰撞差异，记录 first difference 后限定处理，不调 DAT。回滚仅回退本包精确条件；任何删除需文件操作审计和适用授权。
