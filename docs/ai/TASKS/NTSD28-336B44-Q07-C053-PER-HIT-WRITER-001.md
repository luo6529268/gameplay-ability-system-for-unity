# NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001

状态：`VERIFIED`（仅逐hit动作与锁存时序限定出口，C053/Q07开放）。父项：新版 336B44 G1/BATCH-04/Q07/C053 与 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：在已保存的三人自然双 OPoint 同初态样本中，直接观察原 Unity Battle Scene 每一次命中 writer 后 OID808 的目标动作、锁存动作与 HP，并按攻击者槽位逐项对照正式 playable 的两条 `applied/effect2/post-hit action156` 记录。此前已证候选/消费顺序与选定末态，但不能用末态反推两次各自 Uj156。

权威：正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable live path：`battle_world.cpp` 普通 type3 `hit_Uj` 取动作锁存帧及逐 hit `target_type3_post_hit_action`；[正式源码双 Uj CSV](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/source-run-01.csv)。Unity 证据前置与缺口见[逐候选审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-PER-HIT-ORDER-AUDIT-20261002/REPORT.md)。

精确脚本范围：

- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`：只在已有 `ShadowCompare` 的 `ObserveLegacyWriterEffect` 快照点，将实际 target frame、`Trans.WaitCounter` 锁存及 HP 按 entry 缓存，并通过已有 `BattleHitExecutionPlanEntryView` 诊断出口只读暴露；每 tick 预分配 Entry 数组复用，不加战斗分支或帧分配。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs`：独立 request/result 路径与 runId，不覆盖旧结果；原Battle Scene 同三人初态、seed682973786、mode0/difficulty0、12 个完整生产 Driver tick，序列化每个命中 entry 的 writer 后字段并在 tick7 做精确断言。

不修改真实命中 writer、DAT 数值、资源、Scene/Prefab、ProjectSettings、非战斗功能或其它 Q 阶段。保持固定相机与 D-024 双坐标、用户 stage/背景/mode 例外。已有脏文件和原始结果原地保护，不删除、还原或清理。

验收：修改前独立 Task/Change/Ledger/STATE/handoff 均登记；生成 Editor 编译和原 Editor 编译 0 error；原Battle Scene tick7 两条按 slot51→52 各自观测 `Damage`、writer后 target action156／锁存153，tick尾锁存156、HP450、rest各10，12tick所选132个战斗字段及42个待播事件继续与正式源一致；`ShadowCompare` mismatch/failure0；退出Play/Scene clean、四保护SHA稳；运行 `Tools/Validate-ChangeLedger.ps1` 与 `git diff --check`。正式源码通用type3分支只写 `frame.action=response_action`、保留 `frame.action_latch` 到尾部同步，不应把单次post-hit动作误认成即时锁存变化。若根三人初态仍无等价载体，明确保持根 EXE 逐hit不可直接核的边界；物理键自然选招与完整World/Q07总出口不因此关闭。

回滚：审阅本包两处脚本差异后按记录精确回退本包新增诊断字段和探针请求出口；保留全部原始证据与用户工作，不执行未经批准的 Git restore/reset/rm 或文件删除。

脚本已写：entry 在已有 ShadowCompare 实际 writer 快照点保存目标 frame/Trans.WaitCounter/HP，并通过只读 view 导出。首轮原Scene结果保留为 `FIRST_DIFFERENCE`：两次真实 writer 均 action156、锁存153，HP475/450；原测试误期望即时锁存156。当前按正式源码字段写入顺序修正探针，第二轮将使用独立 request、session 与 runId；不覆盖首轮。

验收：第二轮原Battle Scene 12tick SCOPED_PASS / DONE，slot51/52 writer后分别为 156:153:475、156:153:450，tick尾锁存156、HP450，ShadowCompare差异0。正式源码两条post-hit action156；直接战斗132/132、10个待播事件同态。两次生成编译0错，原Editor编译及Play通过、退出clean/四保护SHA稳；首轮错误断言的FIRST_DIFFERENCE保留。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-PER-HIT-WRITER-001/REPORT.md)。根EXE三人同初态、自然物理键、完整World未证。
