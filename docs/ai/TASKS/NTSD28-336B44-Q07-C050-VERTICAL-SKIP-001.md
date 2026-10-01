# NTSD28-336B44-Q07-C050-VERTICAL-SKIP-001

状态：RUNTIME_PENDING；原 Editor 聚焦与完整 Driver 限定对照已过，原 Battle Scene Play/自然键待。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`、G1/BATCH-04/Q07/C050。

当前正式依据：336B44 `BattleWorld28::resolve_confirmed_unarmored_standard_hit` 在特殊链接门 true 时跳过 `accumulate_unarmored_vertical`，仍执行此前伤害/反应计数和后续 rest、post hit。OID24 action37→38 攻击 OID56 action259 的正式完整GameSession tick2 kind0/dvy-5 命中；源/根目标HP465/Vy0/action259。原Unity完整Driver同内容/起始状态 tick2 HP465/Vy0但action186，tick3仍186；远X1200前3tick选定战斗字段无差。详情见 C050 源报告与Unity驱动报告。Unity额外倒地动作归因于 `BattleDamageWriter.ApplyStandardCharacterDamage` 的 `if (knockdown)`尾与垂直调用未受现有 `BattleNativeOrdinaryHitPrelude.IsSpecialLinkRestGate` 约束。

修改范围：`Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs` 仅在普通无甲角色命中路径复用已存在特殊门，使其跳过普通垂直冲量及倒地动作写入；保留计时、伤害、水平反应、rest及命中尾。`Assets/NTSD/Scripts/Test/Editor/NTSD28B5EffectActionOverrideEditorTests.cs` 增加kind50、52与普通BDY的聚焦正反例。不得修改DAT、Scene、资源、非战斗、其它writer或旧夹具数值，不以OID特判。若root/Unity同tick字段揭示更早首差或特殊门时点不符，停下修正范围。

验收：聚焦新正反例在修前RED、修后GREEN；原Editor编译0错；C050近X520与远X1200现有严格新版raw capture各3tick重跑，目标动作、HP、Vy和攻击者动作与正式根逐tick对照；相邻普通未触发门路径聚焦通过。原Battle Scene Play与完整SelfCheck仍单独验，不把raw/EditMode当终局。四保护文件SHA不变。回滚精确两脚本本包行改动及证据需遵守用户工作保护/删除批准规则。

2026-10-01 实施中证据：原Editor修前参数化测试 kind50、kind52 因fall action186而RED，普通kind0 PASS。共用writer已复用既有特殊链接门，修后生成Editor工程`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 0错误/266警告；原Editor正在导入，尚未把GREEN或raw同态写成完成。

2026-10-01 后继结果（覆盖上段待验快照）：原Editor已导入，三例GREEN3/3；近X520与远X1200正式根/原Unity各3tick，两实体动作/HP/Vy每例18/18同态。近距目标由修前tick2/3 action186纠正为259，HP465/Vy0不变。Scene Play、自然按键、完整World仍开放，不能称C050/Q07完成。[对照](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-UNITY-DRIVER-001/REPORT.md)。
