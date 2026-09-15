<!-- CHANGE-RECORD
id: NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001
status: IN_PROGRESS
change-kind: CPOINT_INPUT_SELECTION_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/cpoint_input_action_selection_witness.cpp
authority: Current formal playable battle_world.cpp native relation action and caught-object input selection; default formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: Root read-only caller audit confirms live RunKind1/RunActionSelection and missing selectors despite Q05 data fields; no runtime first-difference claim yet.
-->

# NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001

PLANNED / SOURCE_WITNESS_FIRST.

当前正式BattleWorld28::advance_catch_relations()（5751起）中的attack/throw/defend/previous-depth/facing-dependent front/back/jump选择链，约battle_world.cpp5877..5945；native_relation_action处理负动作翻向；最后非0才写catcher action、按selected native cpoint读victim action并清双方counter。当前Unity BattleCpointWriter.RunKind1 -> RunActionSelection只处理A/T/J且逐项ApplyAction，ApplySignedCpointFrame使用旧SetFrameTickDirect。BattleCatchPointValue已有Daction/Faction/Baction/Uzaction/Dzaction合同，不新增字段或schema。

第一写域仅Tools/NTSD28AuthorityTrace/cpoint_input_action_selection_witness.cpp（新source诊断runner）。确认当前formal EXE/source closure，使用未修改playable源码。覆盖current vs previous输入、edge窗口、左右朝向、8选择入口、同时满足的最终优先级、后续0覆盖取消、正负低/高/隐式/declared999动作和selected victim action、初始及更新CPoint差异。完整before/立即/following raw/B2/counter/latch/snapshot/link/RNG，先独立source模型再Unity RED。

源场景使用throwvx0隔离选择事务，保留其他flags/selector组合；不重做已验CpointThrow392、kind2validation212或已关闭字段退休。捕获same initial relation并保留失败/invalid行，不用Unity定义源规则。当前Unity生产不改；具体测试/生产符号在源证据后另准确追加Record。

验收：源build+双跑+独立检查、Unity两profile完整初态匹配和立即/后继tick、局部replay及真实Play/关闭/新SelfCheck；只达到各层才推进状态。风险：选择顺序、旧输入映射、0取消语义、selected descriptor/vaction与cached initial CPoint后续消费。保持Unity/GAS/33ms/有序关闭和非战斗/资源/Scene边界。回滚仅人工撤销同ID准确diff，保留用户和其它Task修改及失败证据，不自动删除。
