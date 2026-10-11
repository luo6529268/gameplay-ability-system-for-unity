## 2026-10-10 最新阶段与排程（NTSD-OPTIMIZATION-CURRENT-STAGE-PRIORITIES-20261010）

2026-10-10 用户更正：Android真机/GPU认证、IL2CPP/ARM64属于后期；AB部署、Android构建和正式发布随平台交付后置。当前按战斗CPU、CPU侧渲染工作和驻留内存问题排序。原34项ID及历史12高/14中/8低保留，原风险重要性不再冒充当前执行顺序。
当前P1：M-03 → H-08 → H-10；P2：M-12 → M-05 → M-01 → M-02 → M-06。此顺序是评审/投入顺序，不是收益排名，也不代表每项已确认必须改代码。
H07输入约12ms尚未解决、当前无新有据方案而暂缓；H07-LATE已评审暂无必要优化。H06既有成果保留，不续Role精确配对；producer暂缓、95和旧候选不自动开启。只处理原方案，不恢复旧Goal或34项自动队列。
本轮已先评审M03历史证据和当前生产入口。相同显示采样复用已存在；剩余先看命令物化与顶点准备是否有可消除重复，不能因GPU证书未完成就追加设备任务。脚本/资源/设置未改，未跑Unity或新测试/测量，未发送执行指令；监测不恢复。

优先级总表和34单项卡已同步，原证据/暂缓/失败保持。

完整排程：Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md；唯一进度：Assets/NTSD/Docs/battle-optimization-progress-tracker.md；本轮报告：artifacts/diagnostics/NTSD-OPTIMIZATION-CURRENT-STAGE-PRIORITIES-20261010/REPORT.md。

以下均保留此前时点；旧排程与当前状态冲突时以本段及当前总表为准。

---

> **2026-10-10 当前排序：[优先级总清单](../../../Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md)优先于编号顺序。** 首项[H07-INPUT角色输入与AI输入计算](H07-INPUT.md)仅进入实现策略讨论；H-01本地AB后移至Android阶段。已完成/暂缓/待决策/伴随验收分别登记，额外方向不跟进。下方旧H-01或Role“下一项”保留历史，不作为当前排程；未授权实施/运行/派发任务。[决定](DECISIONS.md) · [进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

> **2026-10-10 最新执行边界：[OPTIMIZATION_PLAN_ONLY](DECISIONS.md)。** 用户要求所有优化按既有方案处理，额外发现的方向一律不跟进。当前第一项为[H-01内容部署](../../../Assets/NTSD/Docs/android-mobile-readiness/h-01-content-deployment.md)，仅策略讨论/待确认；不继续H06-ROLE-EXACT诊断，不发送执行指令或恢复监测。下方旧“下一项”均为历史，当前状态以[唯一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)为准。

> **2026-10-10 最新决定：** producer快照复用[用户暂缓/生产OFF](IMPLEMENTATION-RECORDS/H07-PRODUCER-SNAPSHOT-REUSE.md)，下一项[H06-ROLE-EXACT](H06-ROLE-EXACT.md)仅静态分析/方案；Role限定准入成果保持。当前排程以[唯一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)为准，下方旧状态按日期保留；不恢复旧自动目标。

> **2026-10-09 独立实施说明已补充：** [代码前后对照记录与固定格式](IMPLEMENTATION-RECORDS/README.md) · [优化点 A：FindEffect 临时分配修复](IMPLEMENTATION-RECORDS/H06-FIND-EFFECT-ALLOCATION.md)。记录已完成的具体改法及验证范围；当前排程仍以唯一进度为准，不恢复执行。

> 2026-10-09 H06限定准入 BLOCKED / NO_DEFAULT_PROMOTION：四场景32tick逐tick一致性通过；混合OPoint形状collector 8样本各234分配事件，可靠校准有效，默认仍Brute。阻塞不是33ms；Suite/生产代码未改，H06/H07/Android开放，95默认OFF，旧Goal不恢复。 Task `NTSD-H06-ROLE-SCOPED-ADMISSION-20261009` / Change `NTSD-H06-ROLE-SCOPED-ADMISSION-001`。H07-INPUT建议可独立评审，由用户决定实施；不恢复未授权队列。

# 优化文档组织与执行约束（2026-10-09）

状态：DOCUMENTS_PREPARED / D1_A_APPROVED / IMPLEMENTATION_NOT_STARTED。用户授权本会话整理文档，未授权恢复旧自动执行；以下不构成代码、测试、测量或专项启动指令。

## 唯一职责与恢复顺序

1. 先读[当前权威](../CURRENT-AUTHORITY.md)及[AGENTS](../../../AGENTS.md)。
2. [总清单](../../../Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md)文首是全部34父项及6个子方案的导航（2026-10-10增加H06-ROLE-EXACT方案），不能以数量代表收益。
3. [进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)文首是当前/完成范围/下一项。下面批次原文为历史证据。
4. 每个优化点只用原独立方案，文首增加本轮评审卡；原设计/验收/证据不删除。2026-10-10用户新决定取代旧“新热点作为父项子方案”约定：额外发现的优化方向一律不新增、不调查、不实施；既有依赖仅说明，不自动启动。
5. [依赖](DEPENDENCIES.md)统一描述跨项前置；方案卡只摘要。先满足需要的前置阶段，不要求前置父项全关闭。
6. [待决策](DECISIONS.md)记录会改变实施边界的选择。[新会话提示词](NEXT-SESSION-PROMPT.md)已按D1=A定稿，供用户发到新会话启动具名任务。
7. Task/Change Record/FILE-OPERATIONS继续沿用，方案和进度不替代实施审计。

若文首“评审建议”与旧强制验收冲突，以用户最新明确决定及未变更的合同为准；本轮停止授权覆盖旧Goal active叙述。D1=A已批准：既有Role限定准入可先于整体33ms达标；正确性/0GC等门保留，H06父项/H07最终门不变。当前会话不实施，其他工作优先级仍只是建议。PERF/ATLAS/MONO/EXT-1保持原门。

## 真实状态与证据

最新完成94，共73个已完成实施包；95是第74个启动包，验收未完成、默认OFF、用户已取消执行。34父项完整关闭0；首阶段六项5项达到限定实现或评估条件，不是5父项关闭。
已有80共享绑定同Sakura1000 SetPass约1995→5，CPU DrawMesh约2；86选定CPU表现0GC/重进通过。81 Role有实局收益但未推广；91/93实际窗口的声明full Driver steady scope有效0分配，95 Manual/headless资格分配失败不覆盖它们。
角色/平台/指纹、计时范围和fixture必须随结论保存。GC.Alloc事件不等于GC回收停顿；P95不能由均值推导，嵌套相位不能相加。
证据入口：[取消时交接](../HANDOFFS/NTSD-OPTIMIZATION-REVIEW-HANDOFF-20261009.md)、原进度具名80/81/82/86/91/93/95及Change Records。旧记录只按其范围复用，不称本轮复测。

## 本轮读取边界

读取全部34份独立方案的问题/方法/依赖/验收及专项相关章节，结合交接和当前代码定位。新鲜静态核对包括Brute默认resolver、四个Brute fast flag、Role选择入口、共享binding冷接线、Input八子阶段、AI刷新/决策调用点、Stage静态分配候选。不是34项全部生产路径重新审计；其余具体实现仍属于原方案/历史证据，进入实施前核对准确路径。
未读活跃Q06方法体；Late生命周期实现未展开，子方案明确保留未知。没有Unity、测试、Profiler、Frame Debugger、GPU capture或测量。本轮文件哈希只用于保护，不算运行证明。

## 状态与完成边界

- 待分析：缺具体热点/契约，不能直接改。
- 方案待决定：路线已可讨论，授权或合同尚未确定。
- 可执行：准确范围、依赖、预算/门槛、验证及授权均明确；仅“有方案文件”不够。
- 实施中/待验收：须链接具名Task和Change，不能依赖聊天恢复。
- 限定完成：仅关闭已声明范围，父项未覆盖域保持开放。
- 无收益结束：结束具名候选版本，不证明整个技术方向永久无效。
- 依赖阻塞/用户暂缓：写明缺什么和恢复条件；不暂停无关已授权READY项。

方案明确可实施、候选值得继续、H07阶段性能通过、Android最终认证是四种不同结论。实施状态仍使用原Change Record状态，不另建互相矛盾的代码台账。

## 变更、证据复用与有限推进

每次开始一个优化点先冻结：问题、准确路径/符号、预期收益依据、所有权/容量/关闭、依赖、验证与回滚。检查当前dirty字节后备份，不以HEAD替代。
同一行为内的诊断/修复/复测属于同包，不为每个步骤新开编号；真正独立行为或跨越写域才另立Record。新发现按“当前必要修复/真实前置/可选新点/合同变化”分类，后两者不能偷偷并入。
必要性判定：新增实验必须说明现有证据回答不了的具体问题、结果如何改变决定。相同方法已失败且没有改变假设/工具/输入时，不重复刷窗。正确性失败先定位；无收益候选结束采用。不能因为批次数到了就宣告完成，也不能靠无限可选证明延长工作。
复用证据需核对代码及相关依赖哈希、内容/Kernel、配置、平台、workload、测试语义。无关改动不触发全量回归；影响不明只补最小辨别证据。81/82/86/91/93等不机械重跑；受实际默认接线影响的门另列。
现有两布局120+180冒烟、120+1800正式阶段门保持。正式窗口用于验收已具备希望的候选，不用1800长窗掩盖短窗无收益或无效GC采样。
所有新增manager/队列/缓存/worker必须声明11阶段关闭位置和后置条件；性能不得靠改变33ms、pass/RNG/checksum、透明顺序、publication只读、热路径容量或0GC合同实现。

## 更新责任与回滚

单项方案改变时更新该方案及其在总清单/进度的对应行；依赖变化只传播到实际下游阶段。主表记问题与入口，进度记事实与下一动作，方案记方法，Record记实施证据。历史错误追加更正，不删失败。
本轮只前置评审卡，旧正文逐字节保留。备份/前后manifest/检查结果见[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-DOC-REORGANIZATION-20261009/RECORD.md)；撤销已有内容需另获授权，不能自动恢复覆盖后续用户修改。
