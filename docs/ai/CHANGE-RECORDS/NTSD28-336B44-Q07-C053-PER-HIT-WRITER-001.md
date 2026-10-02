<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable battle_world.cpp hit_Uj and same-seed original Battle Scene natural double hit
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001.md
-->

# Q07/C053 每次命中 writer 后目标字段只读观测

脚本前记录。现状：正式 playable 同三人受控初态在 tick7 按槽51→52 两条 `applied/effect2/post-hit action156`；原 Unity Battle Scene 已按相同槽序观测两条 `Damage` 与消费，所选末态/待播事件 174/174，但 entry view 没有逐次 writer 后动作、锁存和 HP。仅凭末态 action156/HP450 不能关闭内部 Uj 时序。

计划改动及所有者：`BattleEcsHitExecutionPlan.ObserveLegacyWriterEffect` 已在 ShadowCompare 中取每次真实 writer 后的 `WriterEffectSnapshot`，在预分配 entry 保存三项实际目标字段并由 `BattleHitExecutionPlanEntryView` 导出；原 C053 Editor Play 探针以新结果路径记录并断言 tick7 两次独立 post-hit action/锁存。无新命中判定、无游戏状态反写，诊断正常非 ShadowCompare 路径不执行。实际路径、函数与状态以 Task 为准。

预期副作用仅为诊断内存中每个 entry 新增少量标量与新 JSON 字段；失败策略保持旧结果不覆盖，并用独立 runId 报 first difference。不可回退边界是已有用户工作、DAT/图片、正式资源、旧证据与原 Battle Scene。验收、风险及精确回滚见 Task；若无法得到原 Editor 编译或 Play，状态保持 `RUNTIME_PENDING`，不冒充 C053/Q07 完成。

脚本后记录：实际只改声明的两个脚本。命中计划 Entry 增加 observed writer target frame/Trans.WaitCounter/HP 三标量和观测标志，使用原有 writer 后快照，无 gameplay 写入；Editor 探针换独立 request/result/session/runId，新增逐条 writer 输出及 tick7 两次 Uj156 断言。编译/Play/四保护 SHA、Change Ledger 验收尚待；C053/Q07与总目标开放。

首轮实际结果及纠正：生成 Editor 编译 exit0、280 warning/0 error；原Editor刷新后两程序集时间晚于本包脚本。原Battle Scene `ank580-ank580-jira500-per-hit-writer-01` 在 tick7 记录 `51:156:153:475;52:156:153:450`，两条 Damage/consume/writer、mismatch/failure0，12tick完成、退出Play/Scene clean、Battle Scene SHA不变。状态 `FIRST_DIFFERENCE` 由探针错误地要求 writer 后 `Trans.WaitCounter=156` 造成，不是已证生产首差。当前正式 `battle_world.cpp` 通用type3分支先读 `frame.action_latch`，再只写 `frame.action=response_action` 与 `frame_counter=0`，没有在该 writer 同步更新 `action_latch`；本案例保留153到tick尾才同步156，Unity第一轮即时字段符合这一顺序。下一在同一声明测试脚本中将断言改为 writer 后action156/锁存153、tick尾156，并用 `-02` 独立 request/session/result；首轮JSON和请求原件不覆盖。正式源码未逐hit公开HP，首轮475/450为Unity直接观测，不能声称对应原生逐hit HP已对照。其它验收待。

限定验收：第二轮新runId原Battle Scene SCOPED_PASS / DONE，12个生产Driver tick、槽51→52逐writer动作均156/即时锁存153、HP475→450，tick尾锁存156/HP450；计划两条Damage/consume/writer差异0。正式源码双applied/effect2/post-hit action156，直接选定战斗132/132零差、10个待播事件逐tick同序。生成工程首轮0错/280 warning、修正后0错/249 warning，原Editor两次刷新后脚本程序集新鲜；两次Play均退出clean，第二轮四保护SHA全同、LoganRuntime无Git差异。首轮错误断言结果保留。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001/REPORT.md)。未验证正式根三人同初态、原生逐hit HP、物理键自然选招、完整World/整场；C053/Q07/总目标继续开放。Change Ledger和diff check最终结果另记录。

文档重复行更正预记录：首次在 docs/ai/CHANGE-LEDGER.md 插入本 ID 时，按通用表格分隔行做全局替换，导致同一新行出现在第5、696、736、1470行共4次。现在只保留首个本 ID 行，精确移除其余3条本包刚误加的重复行；不删其它 Change ID、旧记录、文件或用户内容。修后复核本 ID 恰为1条、Ledger validator和Git diff检查；此处保留原因与数量，防止删除无记录。

交付校验：已精确移除上述3条本包重复行，`rg` 只剩第5行一条；[第二次 Ledger 校验](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001/change-ledger-validation-v2.txt) exit0/PASSED、1125 Records/当前4个受治理脚本全COVERED；`git diff --check` exit0（仅既有行尾转换 warning）。第一轮 Ledger 输出也保留，未覆盖。无文件删除、Git恢复、提交或推送。
