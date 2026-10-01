<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C042-NATURAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs
authority: selected 336B44 OID75 natural source-root LFR evidence and battle-only scope
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C042-NATURAL-SCENE-001.md
-->

# C042 OID75 自然投掷原 Battle Scene 逐tick探针

脚本前记录。现有正式OID75/action355自然抓取、tick55投掷已在源码和根80tick证明，但Unity原Battle Scene未在同初态验；该链投前被投者计数0，不触发C042非零计数分支。只新增独立Play探针和Unity meta，严格限制这一组输入、测60tick，比较原始战斗字段/RNG并保护Scene及用户脏工作。预期只有测试入口，无生产副作用；若发现首差另建生产包。编译、Play、离线比较、Scene clean/四SHA及账本后如实推进状态；回滚只审阅本新增探针。

2026-10-01 实际新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs` 及Unity生成meta；原Editor Tundra编译成功/0 CS error。Play clone独立设正式OID75/2中性初态，经生产Driver60tick得到 CAPTURED/DONE/exitedPlay/sceneCleanAfter；与当前正式源码15实体+5 RNG字段1200/1200零差、首差0，tick54投前计数0/tick55被投者动作181/尾计数1。最终Editor idle非Play非编译、Battle/Menu/GameConfig/ProjectBattleModeConfig四SHA稳。无生产、DAT、Scene、模式Asset、非战斗改动。状态 VERIFIED 仅本条自然投掷路径；C042特定非零计数门、其余入口、Q07和总目标均未证。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-NATURAL-SCENE-001/REPORT.md)。
