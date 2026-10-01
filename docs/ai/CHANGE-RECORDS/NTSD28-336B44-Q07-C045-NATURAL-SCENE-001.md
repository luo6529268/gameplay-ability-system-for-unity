<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C045-NATURAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C045NaturalGrabBattlePlayProbeEditor.cs
authority: selected 336B44 OID65 natural source-root LFR evidence and user battle-only scope
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C045-NATURAL-SCENE-001.md
-->

# C045 OID65 自然抓取原 Battle Scene 逐tick探针

脚本前记录。现有原Battle Scene自然探针可在Play克隆覆盖Roster、暂停并经生产Driver推进完整tick；尚无OID65 action348/357→376抓取/伤害同初态场景记录。只新增独立探针及Unity meta，精确允许348/X550、357/X700、348/X1200三组，记录双方战斗字段与RNG，保留Scene clean和四SHA保护。预期仅测试入口新增，无生产副作用；自然场景若首差则不擅自修生产，另建包。回滚只审阅本新增脚本，不覆盖其他未提交工作。编译、原Editor Play和账本验证后按事实推进状态。

2026-10-01 实际新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C045NaturalGrabBattlePlayProbeEditor.cs` 和Unity生成meta；沿用现有自然探针的只在Play克隆覆盖Roster、暂停等待稳定、明确输入 `StepOneTick`、JSON保存、退出检查Scene哈希结构。原Editor Tundra编译成功/0 CS error。三组各35tick自然运行均 CAPTURED/DONE/exit/clean；与正式源码17实体+5 RNG字段合计2310/2310、首差0，两组抓取伤害和一组远距控制均符合正式根已证的自然入口。最终原Editor idle/非Play/非编译、Battle/Menu/GameConfig/ProjectBattleModeConfig四SHA稳；未改生产、DAT、Scene、配置资产、GAS或非战斗。状态 VERIFIED 仅本三组前35tick字段；全World/全部120tick/物理按键和Q07仍开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-NATURAL-SCENE-001/REPORT.md)。
